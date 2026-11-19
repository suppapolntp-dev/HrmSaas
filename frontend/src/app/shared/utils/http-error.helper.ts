import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/api-response.model';

export function extractProblemDetailsMessage(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as Partial<ProblemDetails> | null;
    if (body?.detail && body.detail.trim().length > 0) {
      return body.detail;
    }
    if (body?.title && body.title.trim().length > 0) {
      return body.title;
    }
    if (body?.errors) {
      for (const messages of Object.values(body.errors)) {
        if (Array.isArray(messages) && messages.length > 0) {
          return messages[0];
        }
      }
    }
  }
  return fallback;
}
