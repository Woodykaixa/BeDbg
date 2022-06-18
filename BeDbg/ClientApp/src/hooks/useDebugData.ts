import { defineStore } from 'pinia';
import { Registers } from '@/dto/thread';
import { Api } from '@/api';
import { ref, UnwrapRef } from 'vue';

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

export const useDebugData = defineStore('debugData', () => {
  const breakPoints = ref(new Set<number>());
  const process = ref(new Map<number, WinProcess>());
  const mainProcess = ref(undefined as any as WinProcess); // CreateProcess event is always triggers before CreateThread event, thus we force case an undefined value here.
  const setBreakpoint = async (address: number) => {
    const { ok, data } = await Api.Breakpoints.set(mainProcess.value.id, address);
    if (ok) {
      breakPoints.value.add(address);
    }
    return data;
  };
  const removeBreakpoint = async (address: number) => {
    const { ok, data } = await Api.Breakpoints.remove(mainProcess.value.id, address);
    if (ok) {
      breakPoints.value.delete(address);
    }
    return data;
  };

  const syncBreakpoints = async () => {
    const { ok, data } = await Api.Breakpoints.list(mainProcess.value.id);
    if (ok) {
      breakPoints.value = new Set(data);
    }
    return data;
  };

  const updateRegisters = async (processId: number, threadId: number) => {
    const { ok, data } = await Api.DebuggingProcess.getRegisters(processId, threadId);
    console.log('update registers', ok, data)
    if (ok) {
      process.value.get(processId)!.threads.get(threadId)!.registers = data;
    }
    return data;
  };
  return {
    breakPoints,
    process,
    mainProcess,
    setBreakpoint,
    removeBreakpoint,
    syncBreakpoints,
    updateRegisters,
  };
});
