import { ProcessModel } from '../dto/process';

export const Api = {
  async getProcessList() {
    const response = await fetch('/api/process');
    if (response.ok) {
      return (await response.json()) as Array<ProcessModel>;
    }
    return null;
  },
} as const;
