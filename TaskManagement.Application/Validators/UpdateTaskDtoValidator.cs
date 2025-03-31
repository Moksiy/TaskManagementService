using FluentValidation;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Validators
{
    /// <summary>
    /// Validator for UpdateTaskDto
    /// </summary>
    public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
    {
        public UpdateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters")
                .When(x => x.Title != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
                .When(x => x.Description != null);

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: New, InProgress, Completed")
                .When(x => !string.IsNullOrEmpty(x.Status));
        }

        private bool BeValidStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return true;

            return Enum.TryParse<TaskStatus>(status, true, out _);
        }
    }
}
