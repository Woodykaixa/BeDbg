import { defineStore } from 'pinia';

export type X64Instruction = {
  address: number;
  text: string;
};

export type WinThread = {
  id: number;
  address: number;
  entry: number;
  instructions: X64Instruction[];
};

export type WinProcess = {
  id: number;
  threads: Map<number, WinThread>;
  mainThread: WinThread;
};

export type WinBreakPoint = {
  address: number;
  originalInstruction: number;
};

export const useDebugData = defineStore('debugData', {
  state: () => {
    return {
      process: new Map<number, WinProcess>(),
      mainProcess: undefined as any as WinProcess, // CreateProcess event is always triggers before CreateThread event, thus we force case an undefined value here.
      breakPoints: [] as WinBreakPoint[],
    };
  },
});
