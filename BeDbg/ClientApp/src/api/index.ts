import { DirectoryModel, FileModel } from '@/dto/fs';
import { ProcessModel } from '../dto/process';

export const Api = {
  async attachProcess(pid: number) {
    const resp = await fetch('/api/process/attach', {
      method: 'POST',
      body: pid.toString(10),
      headers: {
        'content-type': 'application/json',
        accept: 'application/json',
      },
    });

    if (resp.ok) {
      return (await resp.json()) as number;
    }
    return 0;
  },

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

  async createProcess(file: string, command: string) {
    const response = await fetch('/api/process', {
      method: 'POST',
      body: JSON.stringify({
        file,
        command,
      }),
      headers: {
        'content-type': 'application/json',
      },
    });
    if (response.ok) {
      return (await response.json()) as ProcessModel;
    }
    return null;
  },

  async getFileList(dir?: string) {
    const response = await fetch(dir ? `/api/fs/ls?dir=${dir}` : '/api/fs/ls');
    if (response.ok) {
      return (await response.json()) as DirectoryModel;
    }
    return null;
  },
} as const;
