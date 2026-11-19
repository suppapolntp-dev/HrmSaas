import { FormControl, FormGroup } from '@angular/forms';
import { ProblemDetails } from '../models/api-response.model';
import { applyProblemDetailsErrors } from './form-error.helper';

describe('applyProblemDetailsErrors', () => {
  it('should map field errors case-insensitively and return unmapped errors', () => {
    const form = new FormGroup({
      email: new FormControl(''),
      password: new FormControl(''),
    });

    const problem: ProblemDetails = {
      title: 'Validation Failed',
      status: 400,
      errors: {
        Email: ['รูปแบบอีเมลไม่ถูกต้อง'],
        General: ['บัญชีถูกระงับชั่วคราว'],
      },
    };

    const unmapped = applyProblemDetailsErrors(problem, form);

    expect(form.controls.email.errors?.['serverError']).toBe('รูปแบบอีเมลไม่ถูกต้อง');
    expect(form.controls.email.touched).toBe(true);
    expect(form.controls.password.errors).toBeNull();
    expect(unmapped).toEqual(['บัญชีถูกระงับชั่วคราว']);
  });
});
