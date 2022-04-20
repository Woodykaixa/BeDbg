import { ErrorResponse } from '@/dto/error';
import { DebuggingProcess as DebuggingProcessResp, ProcessMemoryPage, ProcessModule } from '@/dto/process';
import { apiRequestWrapper } from '@/util/request';

export const DebuggingProcess = {
  async list() {
    return apiRequestWrapper<DebuggingProcessResp[]>(
      fetch('/api/debuggingProcess', {
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
      })
    );
  },
  async attachProcess(pid: number) {
    return apiRequestWrapper<DebuggingProcessResp>(
      fetch(`/api/debuggingProcess/attach?pid=${pid}`, {
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
      })
    );
  },

  async detachProcess(pid: number) {
    return apiRequestWrapper<null>(
      fetch(`/api/debuggingProcess/detach`, {
        method: 'POST',
        body: JSON.stringify(pid),
        headers: {
          'content-type': 'application/json',
          accept: 'application/json',
        },
      }),
      true
    );
  },

  async create(file: string, command: string) {
    return apiRequestWrapper<number>(
      fetch('/api/debuggingProcess', {
        method: 'POST',
        body: JSON.stringify({
          file,
          command,
        }),
        headers: {
          'content-type': 'application/json',
        },
      })
    );
  },

  async listModules(pid: number) {
    return apiRequestWrapper<ProcessModule[]>(fetch(`api/debuggingProcess/modules?pid=${pid}`));
  },

  async listPages(pid: number) {
    return apiRequestWrapper<ProcessMemoryPage[]>(fetch('/api/debuggingProcess/pages?pid=' + pid));
  },

  async disassemble(pid: number, address: number) {
    return apiRequestWrapper<{ ip: number; text: string }[]>(
      fetch('/api/debuggingProcess/disasm/' + pid + '?address=' + address)
    );
  },
} as const;
