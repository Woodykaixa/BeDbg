export function inputFileAsync(): Promise<FileList> {
  return new Promise((resolve, reject) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.click();
    input.addEventListener('input', () => {
      if (input.files) {
        resolve(input.files);
      } else {
        reject(new InputCanceledError());
      }
    });
  });
}

class InputCanceledError extends Error {
  constructor() {
    super('Input canceled');
  }
}
