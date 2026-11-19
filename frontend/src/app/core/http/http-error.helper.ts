import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../../shared/models/api-response.model';

export function extractProblemDetails(error: unknown): ProblemDetails {
  if (error instanceof HttpErrorResponse) {
    const payload = error.error;
    if (isProblemDetailsPayload(payload)) {
      return {
        type: payload.type,
        title: payload.title,
        status: payload.status || error.status,
        detail: payload.detail,
        instance: payload.instance,
        traceId: payload.traceId,
        errors: payload.errors,
      };
    }

    if (error.status === 0) {
      return {
        title: 'ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์ได้',
        status: 0,
        detail: 'กรุณาตรวจสอบว่า Backend API กำลังทำงานอยู่แล้วลองใหม่อีกครั้ง',
      };
    }

    return {
      title: error.statusText || 'เกิดข้อผิดพลาดในการเรียกใช้งานระบบ',
      status: error.status,
      detail: typeof payload === 'string' && payload.length > 0 ? payload : error.message,
    };
  }

  if (isProblemDetailsPayload(error)) {
    return error;
  }

  return {
    title: 'เกิดข้อผิดพลาดที่ไม่คาดคิด',
    status: 500,
    detail: 'กรุณาลองใหม่อีกครั้ง',
  };
}

function isProblemDetailsPayload(value: unknown): value is ProblemDetails {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Partial<ProblemDetails>;
  return typeof candidate.title === 'string' && typeof candidate.status === 'number';
}
