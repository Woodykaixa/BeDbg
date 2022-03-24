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
