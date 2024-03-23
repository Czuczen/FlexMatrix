using FlexMatrix.Api.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FlexMatrix.Api.Data.Validators
{
    public class TableStructureValidator : IValidator<TableStructureDto>
    {
        public void Validate(TableStructureDto tableStructure)
        {
            var errors = new List<string>();

            // Walidacja nazwy tabeli
            if (string.IsNullOrWhiteSpace(tableStructure.TableName))
            {
                errors.Add("Table name cannot be empty.");
            }

            // Walidacja unikalności nazw kolumn
            var columnNames = new HashSet<string>();
            foreach (var column in tableStructure.Columns)
            {
                if (!columnNames.Add(column.Name))
                {
                    errors.Add($"Duplicate column name detected: {column.Name}.");
                }

                // Tutaj mogą znaleźć się inne reguły walidacji
                if (string.IsNullOrWhiteSpace(column.Type))
                {
                    errors.Add($"Column type for {column.Name} cannot be empty.");
                }
            }

            // Jeżeli wykryto błędy, rzuć wyjątek
            if (errors.Any())
            {
                throw new ValidationException(string.Join(Environment.NewLine, errors));
            }
        }
    }
}
