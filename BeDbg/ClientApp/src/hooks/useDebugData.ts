import { defineStore } from 'pinia';
import { Registers } from '@/dto/thread';
import { Api } from '@/api';

export type X64Instruction = {
  address: number;
  text: string;
};

export type WinThread = {
  id: number;
  address: number;
  entry: number;
  instructions: X64Instruction[];
  registers: Registers;
};

export type WinProcess = {
  id: number;
  threads: Map<number, WinThread>;
  mainThread: WinThread;
};

export const useDebugData = defineStore('debugData', {
  state: () => {
    return {
      breakPoints: new Set<number>(),
      process: new Map<number, WinProcess>(),
      mainProcess: undefined as any as WinProcess, // CreateProcess event is always triggers before CreateThread event, thus we force case an undefined value here.
    };
  },
  actions: {
    async setBreakpoint(address: number) {
      const { ok, data } = await Api.Breakpoints.set(this.mainProcess.id, address);
      if (ok) {
        this.breakPoints.add(address);
      }
      return data;
    },
    async removeBreakpoint(address: number) {
      const { ok, data } = await Api.Breakpoints.remove(this.mainProcess.id, address);
      if (ok) {
        this.breakPoints.delete(address);
      }
      return data;
    },
    async updateRegisters(pid: number, tid: number) {
      const { ok, data } = await Api.DebuggingProcess.getRegisters(pid, tid);
      if (ok) {
        this.process.get(pid)!.threads.get(tid)!.registers = data;
        console.log('update success');
        return { ok };
      } else {
        console.log('update failed');
        return { ok, data };
      }
    },
    async syncBreakpoints() {
      const { ok, data } = await Api.Breakpoints.list(this.mainProcess.id);
      if (ok) {
        this.breakPoints = new Set(data);
      }
      return data;
    }
  },
});
