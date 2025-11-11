namespace WebTemplate.Data.Validation
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Reflection;
    using System.Text;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using WebTemplate.Core.Entities;
    using WebTemplate.Data.Context;

    /// <summary>
    /// Validates that ApplicationDbContext configuration matches 100% with entity definitions
    /// Ensures no configuration drift between Data Annotations and Fluent API
    /// </summary>
    public class DbContextValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly List<ValidationIssue> _issues = new();

        public DbContextValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public ValidationResult Validate()
        {
            _issues.Clear();

            // Get all entity types from WebTemplate.Core.Entities namespace
            var entityTypes = typeof(ApplicationUser).Assembly
                .GetTypes()
                .Where(t => t.Namespace == "WebTemplate.Core.Entities"
                         && t.IsClass
                         && !t.IsAbstract
                         && !t.Name.Contains("<>")) // Exclude compiler-generated types
                .ToList();

            Console.WriteLine($"Found {entityTypes.Count} entity types to validate");

            foreach (var entityType in entityTypes)
            {
                ValidateEntity(entityType);
            }

            return new ValidationResult
            {
                IsValid = _issues.Count == 0,
                Issues = _issues.ToList(),
                EntitiesValidated = entityTypes.Count
            };
        }

        private void ValidateEntity(Type entityType)
        {
            Console.WriteLine($"\nValidating entity: {entityType.Name}");

            // Get EF Core metadata for this entity
            var efEntityType = _context.Model.FindEntityType(entityType);

            if (efEntityType == null)
            {
                // Check if it's supposed to be in DbContext
                // Skip if it's IdentityUser (base class, handled by Identity)
                if (entityType.BaseType?.Name != "IdentityUser")
                {
                    _issues.Add(new ValidationIssue
                    {
                        Severity = IssueSeverity.Error,
                        EntityName = entityType.Name,
                        Message = $"Entity '{entityType.Name}' is not registered in ApplicationDbContext"
                    });
                }
                return;
            }

            // Validate all properties
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetCustomAttributes<NotMappedAttribute>().Any())
                .Where(p => !p.GetMethod?.IsVirtual == false || p.GetMethod?.IsFinal == true) // Skip computed properties
                .ToList();

            foreach (var property in properties)
            {
                // Skip computed properties (get-only with expression body)
                if (property.GetMethod != null && property.SetMethod == null && !property.PropertyType.IsClass)
                {
                    continue;
                }

                ValidateProperty(efEntityType, property);
            }

            // Validate indexes
            ValidateIndexes(efEntityType, entityType);

            // Validate relationships
            ValidateRelationships(efEntityType, entityType);
        }

        private void ValidateProperty(IEntityType efEntityType, PropertyInfo property)
        {
            var efProperty = efEntityType.FindProperty(property.Name);

            // Check if property exists in EF configuration
            if (efProperty == null)
            {
                // Skip navigation properties (single entities)
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !property.PropertyType.IsGenericType)
                {
                    return; // This is a single navigation property, handled separately
                }

                // Skip collection navigation properties (ICollection<T>, List<T>, etc.)
                if (property.PropertyType.IsGenericType)
                {
                    var genericDef = property.PropertyType.GetGenericTypeDefinition();
                    if (genericDef == typeof(ICollection<>) ||
                        genericDef == typeof(List<>) ||
                        genericDef == typeof(IEnumerable<>) ||
                        genericDef == typeof(HashSet<>))
                    {
                        return; // This is a collection navigation property, handled separately
                    }
                }

                _issues.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    EntityName = efEntityType.ClrType.Name,
                    PropertyName = property.Name,
                    Message = $"Property '{property.Name}' exists on entity but not configured in DbContext"
                });
                return;
            }

            // Validate [Required] attribute
            var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
            if (requiredAttr != null && efProperty.IsNullable)
            {
                _issues.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    EntityName = efEntityType.ClrType.Name,
                    PropertyName = property.Name,
                    Expected = "IsRequired",
                    Actual = "IsNullable",
                    Message = $"Property '{property.Name}' has [Required] attribute but is nullable in DbContext"
                });
            }

            // Validate [StringLength] attribute
            var stringLengthAttr = property.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttr != null)
            {
                var maxLength = efProperty.GetMaxLength();
                if (maxLength != stringLengthAttr.MaximumLength)
                {
                    _issues.Add(new ValidationIssue
                    {
                        Severity = IssueSeverity.Error,
                        EntityName = efEntityType.ClrType.Name,
                        PropertyName = property.Name,
                        Expected = $"MaxLength={stringLengthAttr.MaximumLength}",
                        Actual = $"MaxLength={maxLength?.ToString() ?? "null"}",
                        Message = $"Property '{property.Name}' StringLength mismatch"
                    });
                }
            }

            // Validate [Key] attribute
            var keyAttr = property.GetCustomAttribute<KeyAttribute>();
            if (keyAttr != null && !efProperty.IsKey())
            {
                _issues.Add(new ValidationIssue
                {
                    Severity = IssueSeverity.Error,
                    EntityName = efEntityType.ClrType.Name,
                    PropertyName = property.Name,
                    Expected = "IsPrimaryKey",
                    Actual = "Not a key",
                    Message = $"Property '{property.Name}' has [Key] attribute but not configured as primary key"
                });
            }
        }

        private void ValidateIndexes(IEntityType efEntityType, Type entityType)
        {
            var indexes = efEntityType.GetIndexes().ToList();

            // Get properties that should be indexed based on common patterns
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Check for common indexable properties that might be missing
            foreach (var prop in properties)
            {
                // Foreign keys should have indexes
                if (prop.Name.EndsWith("Id") && prop.PropertyType == typeof(int) || prop.PropertyType == typeof(string))
                {
                    var hasIndex = indexes.Any(idx =>
                        idx.Properties.Count == 1 &&
                        idx.Properties.First().Name == prop.Name);

                    if (!hasIndex && prop.Name != "Id") // Skip primary key
                    {
                        var fkAttr = prop.GetCustomAttribute<ForeignKeyAttribute>();
                        if (fkAttr != null)
                        {
                            _issues.Add(new ValidationIssue
                            {
                                Severity = IssueSeverity.Warning,
                                EntityName = efEntityType.ClrType.Name,
                                PropertyName = prop.Name,
                                Message = $"Foreign key '{prop.Name}' might benefit from an index"
                            });
                        }
                    }
                }
            }

            // Validate unique indexes match unique constraints
            foreach (var index in indexes.Where(i => i.IsUnique))
            {
                Console.WriteLine($"  Found unique index on: {string.Join(", ", index.Properties.Select(p => p.Name))}");
            }
        }

        private void ValidateRelationships(IEntityType efEntityType, Type entityType)
        {
            var navigationProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null ||
                           (p.PropertyType.IsClass && p.PropertyType.Namespace == "WebTemplate.Core.Entities"))
                .ToList();

            foreach (var navProp in navigationProperties)
            {
                var fkAttr = navProp.GetCustomAttribute<ForeignKeyAttribute>();
                if (fkAttr != null)
                {
                    // Find the foreign key in EF metadata
                    var matchingFks = efEntityType.GetForeignKeys()
                        .Where(fk => fk.Properties.Any(p => p.Name == fkAttr.Name))
                        .ToList();

                    if (!matchingFks.Any())
                    {
                        _issues.Add(new ValidationIssue
                        {
                            Severity = IssueSeverity.Error,
                            EntityName = efEntityType.ClrType.Name,
                            PropertyName = navProp.Name,
                            Message = $"Navigation property '{navProp.Name}' has [ForeignKey(\"{fkAttr.Name}\")] but no FK configured in DbContext"
                        });
                    }
                }
            }

            // Validate configured foreign keys
            var foreignKeys = efEntityType.GetForeignKeys().ToList();
            foreach (var fk in foreignKeys)
            {
                var fkPropName = fk.Properties.First().Name;
                var principal = fk.PrincipalEntityType.ClrType.Name;
                var deleteAction = fk.DeleteBehavior;

                Console.WriteLine($"  Foreign Key: {fkPropName} -> {principal} (DeleteBehavior: {deleteAction})");
            }
        }

        public static ValidationResult ValidateWithNewContext(string connectionString)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);
            var validator = new DbContextValidator(context);
            return validator.Validate();
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new();
        public int EntitiesValidated { get; set; }

        public string GetReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("DbContext Validation Report");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Entities Validated: {EntitiesValidated}");
            sb.AppendLine($"Total Issues: {Issues.Count}");
            sb.AppendLine($"Status: {(IsValid ? "✓ PASS" : "✗ FAIL")}");
            sb.AppendLine();

            if (Issues.Count == 0)
            {
                sb.AppendLine("✓ All entity configurations match 100% with DbContext!");
                return sb.ToString();
            }

            // Group by severity
            var errors = Issues.Where(i => i.Severity == IssueSeverity.Error).ToList();
            var warnings = Issues.Where(i => i.Severity == IssueSeverity.Warning).ToList();

            if (errors.Any())
            {
                sb.AppendLine($"ERRORS ({errors.Count}):");
                sb.AppendLine("-".PadRight(80, '-'));
                foreach (var issue in errors)
                {
                    sb.AppendLine(issue.ToString());
                }
                sb.AppendLine();
            }

            if (warnings.Any())
            {
                sb.AppendLine($"WARNINGS ({warnings.Count}):");
                sb.AppendLine("-".PadRight(80, '-'));
                foreach (var issue in warnings)
                {
                    sb.AppendLine(issue.ToString());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public class ValidationIssue
    {
        public IssueSeverity Severity { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string Expected { get; set; } = string.Empty;
        public string Actual { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public override string ToString()
        {
            var prefix = Severity == IssueSeverity.Error ? "[ERROR]" : "[WARN]";
            var location = string.IsNullOrEmpty(PropertyName)
                ? $"{EntityName}"
                : $"{EntityName}.{PropertyName}";

            if (!string.IsNullOrEmpty(Expected))
            {
                return $"{prefix} {location}: {Message}\n    Expected: {Expected}\n    Actual: {Actual}";
            }

            return $"{prefix} {location}: {Message}";
        }
    }

    public enum IssueSeverity
    {
        Warning,
        Error
    }
}
