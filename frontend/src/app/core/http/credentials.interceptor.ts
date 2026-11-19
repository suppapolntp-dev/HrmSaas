import { HttpInterceptorFn } from '@angular/common/http';

const XSRF_COOKIE_NAME = 'XSRF-TOKEN';
const XSRF_HEADER_NAME = 'X-XSRF-TOKEN';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  const method = req.method.toUpperCase();
  const isStateChanging =
    method === 'POST' || method === 'PUT' || method === 'PATCH' || method === 'DELETE';

  if (!isStateChanging) {
    return next(req.clone({ withCredentials: true }));
  }

  const xsrfToken = readCookieValue(XSRF_COOKIE_NAME);
  if (xsrfToken !== null && xsrfToken.length > 0) {
    return next(
      req.clone({
        withCredentials: true,
        setHeaders: {
          [XSRF_HEADER_NAME]: xsrfToken,
        },
      }),
    );
  }

  return next(req.clone({ withCredentials: true }));
};

function readCookieValue(cookieName: string): string | null {
  if (typeof document === 'undefined') {
    return null;
  }

  const pairs = document.cookie.split(';');
  for (const rawPair of pairs) {
    const trimmed = rawPair.trim();
    const prefix = `${cookieName}=`;
    if (trimmed.startsWith(prefix)) {
      const rawValue = trimmed.slice(prefix.length);
      return decodeURIComponent(rawValue);
    }
  }

  return null;
}
