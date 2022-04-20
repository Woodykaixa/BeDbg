export const contextValues = [
  'RAX',
  'RBX',
  'RCX',
  'RDX',
  'RBP',
  'RSP',
  'RSI',
  'RDI',
  'R8',
  'R9',
  'R10',
  'R11',
  'R12',
  'R13',
  'R14',
  'R15',
  'RIP',
] as const;

export type ThreadContext = {
  [key in typeof contextValues[number]]: number;
};
