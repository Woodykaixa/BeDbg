export type Number128 = {
  low: number;
  high: number;
};
export const GeneralPurposeRegisters = [
  'rax',
  'rcx',
  'rdx',
  'rbx',
  'rsp',
  'rbp',
  'rsi',
  'rdi',
  'r8',
  'r9',
  'r10',
  'r11',
  'r12',
  'r13',
  'r14',
  'r15',
] as const;

export const SegmentRegisters = ['segCs', 'segDs', 'segEs', 'segFs', 'segGs', 'segSs'] as const;

export const DebugRegisters = ['dr0', 'dr1', 'dr2', 'dr3', 'dr6', 'dr7'] as const;

export const FpuStackRegisters = ['st0', 'st1', 'st2', 'st3', 'st4', 'st5', 'st6', 'st7'] as const;

export const SimdRegisters = [
  'xmm0',
  'xmm1',
  'xmm2',
  'xmm3',
  'xmm4',
  'xmm5',
  'xmm6',
  'xmm7',
  'xmm8',
  'xmm9',
  'xmm10',
  'xmm11',
  'xmm12',
  'xmm13',
  'xmm14',
  'xmm15',
] as const;

export type Registers = {
  // Flag registers
  rip: number;
  mxCsr: number;
  eFlags: number;
  fpuTagWord: number;
  fpuStatusWord: number;
  fpuControlWord: number;
} & {
  [key in typeof GeneralPurposeRegisters[number]]: number; // General purpose registers
} & {
  [key in typeof SegmentRegisters[number]]: number; // Segment registers
} & {
  [key in typeof DebugRegisters[number]]: number; // Debug registers
} & {
  [key in typeof FpuStackRegisters[number]]: Number128; // FPU stack registers
} & {
  [key in typeof SimdRegisters[number]]: Number128; // SIMD registers
};

const MapRegisterByName = <
  RegisterValueType extends number | Number128,
  RegisterName extends string,
  RegisterList extends Readonly<RegisterName[]>
>(
  registers: RegisterList,
  mapper: (reg: RegisterList[number]) => RegisterValueType
) => {
  return Object.fromEntries(registers.map(name => [name, mapper(name)])) as {
    [key in RegisterList[number]]: RegisterValueType;
  };
};

export const DefaultEmptyRegisters: Registers = {
  rip: 0,
  mxCsr: 0,
  eFlags: 0,
  fpuTagWord: 0,
  fpuStatusWord: 0,
  fpuControlWord: 0,
  ...MapRegisterByName(GeneralPurposeRegisters, n => 0),
  ...MapRegisterByName(SegmentRegisters, n => 0),
  ...MapRegisterByName(DebugRegisters, n => 0),
  ...MapRegisterByName(FpuStackRegisters, () => ({ low: 0, high: 0 })),
  ...MapRegisterByName(SimdRegisters, () => ({ low: 0, high: 0 })),
};
