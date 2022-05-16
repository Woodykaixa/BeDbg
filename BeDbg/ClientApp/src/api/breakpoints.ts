import { apiRequestWrapper } from '@/util/request';

export const Breakpoints = {
  async list(pid: number) {
    return apiRequestWrapper<number[]>(
      fetch(`/api/debuggingProcess/bp/${pid}`, {
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
      })
    );
  },
  async set(pid: number, address: number) {
    return apiRequestWrapper<void>(
      fetch(`/api/debuggingProcess/bp/${pid}`, {
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
        method: 'POST',
        body: JSON.stringify(address),
      }),
      true
    );
  },

  async has(pid: number, address: number) {
    return apiRequestWrapper<boolean>(
      fetch(`/api/debuggingProcess/bp/${pid}/${address}`, {
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
      })
    );
  },

  async remove(pid: number, address: number) {
    return apiRequestWrapper<void>(
      fetch(`/api/debuggingProcess/bp/${pid}`, {
        method: 'DELETE',
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
        body: JSON.stringify(address),
      }),
      true
    );
  },
} as const;

