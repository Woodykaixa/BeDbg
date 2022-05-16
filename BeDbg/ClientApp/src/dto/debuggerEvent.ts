export type DebuggerEventPayload = {
  process: number;
  thread: number;
};

export type ExceptionPayload = DebuggerEventPayload & {
  exceptionCode: number;
  exceptionFlag: number;
  exceptionAddress: number;
  firstChance: number;
};

export type CreateThreadPayload = DebuggerEventPayload & {
  threadLocalBase: number;
  startAddress: number;
};

export type CreateProcessPayload = DebuggerEventPayload & {
  threadLocalBase: number;
  startAddress: number;
  baseOfImage: number;
};

export type ExitThreadPayload = DebuggerEventPayload & {
  exitCode: number;
};

export type ExitProcessPayload = ExitThreadPayload;

export type LoadDllPayload = DebuggerEventPayload & {
  baseOfDll: number;
};

export type UnloadDllPayload = DebuggerEventPayload & {
  baseOfDll: number;
};

export type OutputDebugStringPayload = DebuggerEventPayload;

export type RipPayload = DebuggerEventPayload & {
  error: number;
  type: number;
};

export const DebuggerEventTypes = [
  'exception',
  'createThread',
  'createProcess',
  'exitThread',
  'exitProcess',
  'loadDll',
  'unloadDll',
  'outputDebugString',
  'rip',
  'exitProgram',
  'programReady',
  'notFound',
  'breakpoint',
  'singleStep',
] as const;

export type DebuggerEvents = {
  exception: [ExceptionPayload];
  createThread: [CreateThreadPayload];
  createProcess: [CreateProcessPayload];
  exitThread: [ExitThreadPayload];
  exitProcess: [ExitProcessPayload];
  loadDll: [LoadDllPayload];
  unloadDll: [UnloadDllPayload];
  outputDebugString: [OutputDebugStringPayload];
  rip: [RipPayload];
  exitProgram: [];
  programReady: [];
  notFound: [];
  breakpoint: [ExceptionPayload];
  singleStep: [ExceptionPayload];
};
