import { ErrorResponse } from '@/dto/error';
import { DebuggingProcess as DebuggingProcessResp } from '@/dto/process';

export const DebuggingProcess = {
  async list() {
    const resp = await fetch('/api/debuggingProcess', {
      headers: {
        'content-type': 'application/json',
        accept: 'application/json',
      },
    });
    const json = await resp.json();
    if (resp.ok) {
      return {
        ok: true,
        data: json as DebuggingProcessResp[],
      } as const;
    }
    return {
      ok: false,
      data: json as ErrorResponse,
    } as const;
  },
  async attachProcess(pid: number) {
    const resp = await fetch(`/api/debuggingProcess/attach?pid=${pid}`, {
      headers: {
        'content-type': 'application/json',
        accept: 'application/json',
      },
    });
    const json = await resp.json();
    if (resp.ok) {
      return {
        ok: true,
        data: json as DebuggingProcessResp,
      } as const;
    }
    return {
      ok: false,
      data: json as ErrorResponse,
    } as const;
  },

  async detachProcess(pid: number) {
    const resp = await fetch(`/api/debuggingProcess/detach`, {
      method: 'POST',
      body: JSON.stringify(pid),
      headers: {
        'content-type': 'application/json',
        accept: 'application/json',
      },
    });
    if (resp.ok) {
      return {
        ok: true,
        data: null,
      } as const;
    }
    return {
      ok: false,
      data: (await resp.json()) as ErrorResponse,
    } as const;
  },
} as const;
