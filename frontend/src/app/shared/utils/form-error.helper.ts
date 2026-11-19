import { AbstractControl, FormGroup } from '@angular/forms';
import { ProblemDetails } from '../models/api-response.model';

/**
 * Maps RFC 7807 validation errors onto matching Angular form controls
 * and returns any unmapped error messages for summary display.
 */
export function applyProblemDetailsErrors(
  problemDetails: ProblemDetails,
  form: FormGroup,
): string[] {
  const unmappedMessages: string[] = [];
  const errorsMap = problemDetails.errors;

  if (!errorsMap) {
    if (problemDetails.detail && problemDetails.detail.trim().length > 0) {
      unmappedMessages.push(problemDetails.detail);
    } else if (problemDetails.title.trim().length > 0) {
      unmappedMessages.push(problemDetails.title);
    }
    return unmappedMessages;
  }

  const controlLookup = buildCaseInsensitiveControlMap(form);
  const errorEntries = Object.entries(errorsMap);

  for (const [fieldKey, messages] of errorEntries) {
    if (!Array.isArray(messages) || messages.length === 0) {
      continue;
    }

    const normalizedKey = fieldKey.trim().toLowerCase();
    const targetControl = controlLookup.get(normalizedKey);

    if (targetControl) {
      const existingErrors = targetControl.errors ?? {};
      targetControl.setErrors({
        ...existingErrors,
        serverError: messages[0],
      });
      targetControl.markAsTouched();
    } else {
      for (const message of messages) {
        unmappedMessages.push(message);
      }
    }
  }

  if (unmappedMessages.length === 0 && errorEntries.length === 0) {
    unmappedMessages.push(problemDetails.detail ?? problemDetails.title);
  }

  return unmappedMessages;
}

function buildCaseInsensitiveControlMap(form: FormGroup): Map<string, AbstractControl> {
  const map = new Map<string, AbstractControl>();
  const entries = Object.entries(form.controls);

  for (const [controlName, control] of entries) {
    map.set(controlName.toLowerCase(), control);
  }

  return map;
}
