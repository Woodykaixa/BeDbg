export type FileModel = {
  name: string;
  type: 'file' | 'folder';
  path: string;
};

export type DirectoryModel = {
  path: string;
  files: FileModel[];
};
