export type ProcessModel = {
  name: string;
  id: number;
  title: string;
  wow64: boolean;
  command: string;
};

export type DebuggingProcess = {
  id: number;
  handle: number;
  attachTime: Date;
};

export type ProcessModule = {
  name: string;
  entry: number;
  size: number;
  base: number;
};

export type MemoryProtection = {
  copy: boolean;
  execute: boolean;
  guard: boolean;
  read: boolean;
  write: boolean;
};

export type ProcessMemoryPage = {
  allocAddress: number;
  baseAddress: number;
  flags: MemoryProtection;
  initialFlags: MemoryProtection;
  size: number;
  state: number;
  type: number;
};
