import { DirectoryModel, FileModel } from '@/dto/fs';
import { ProcessModel } from '../dto/process';

export const Api = {
  async getProcessList() {
    const response = await fetch('/api/process');
    if (response.ok) {
      return (await response.json()) as Array<ProcessModel>;
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
