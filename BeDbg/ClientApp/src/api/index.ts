import { ErrorResponse } from '@/dto/error';
import { DirectoryModel, FileModel } from '@/dto/fs';
import { ProcessModel } from '../dto/process';
import { DebuggingProcess } from './debuggingProcess';
export const Api = {
  DebuggingProcess,
  async getProcessList() {
    const response = await fetch('/api/process');
    if (response.ok) {
      return (await response.json()) as Array<ProcessModel>;
    }
    return null;
  },

  async readProcessMemory({ address, size, pid }: { address: number; size: number; pid: number }) {
    const response = await fetch(`/api/process/read?address=${address}&pid=${pid}&size=${size}`);
    if (response.ok) {
      return await response.blob();
    }
    return null;
  },

  async getFileList(dir?: string) {
    const response = await fetch(dir ? `/api/fs/ls?dir=${dir}` : '/api/fs/ls', {
      headers: {
        'content-type': 'application/json',
        accept: 'application/json',
      },
    });
    const json = await response.json();
    if (response.ok) {
      return {
        ok: true,
        data: json as DirectoryModel,
      } as const;
    }
    return {
      ok: false,
      data: json as ErrorResponse,
    } as const;
  },
} as const;
