import { ErrorResponse } from '@/dto/error';

export async function apiRequestWrapper<T>(response: Promise<Response>, noData: boolean = false) {
  const resp = await response;
  const json = noData ? null : await resp.json();
  if (resp.ok) {
    return {
      ok: true,
      data: json as T,
    } as const;
  }
  return {
    ok: false,
    data: json as ErrorResponse,
  } as const;
}
