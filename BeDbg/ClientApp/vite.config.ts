import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import * as fs from 'fs';
import * as path from 'path';
import { resolve } from 'path';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const certBaseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;
if (!certificateName) {
  console.error(
    'Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.'
  );
  process.exit(-1);
}

const certFilePath = path.join(certBaseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(certBaseFolder, `${certificateName}.key`);

if (!fs.existsSync('.env.development.local')) {
  fs.writeFileSync(
    '.env.development.local',
    `SSL_CRT_FILE=${certFilePath}
SSL_KEY_FILE=${keyFilePath}`
  );
} else {
  let lines = fs.readFileSync('.env.development.local').toString().split('\n');

  let hasCert,
    hasCertKey = false;
  for (const line of lines) {
    if (/SSL_CRT_FILE=.*/i.test(line)) {
      hasCert = true;
    }
    if (/SSL_KEY_FILE=.*/i.test(line)) {
      hasCertKey = true;
    }
  }
  if (!hasCert) {
    fs.appendFileSync('.env.development.local', `\nSSL_CRT_FILE=${certFilePath}`);
  }
  if (!hasCertKey) {
    fs.appendFileSync('.env.development.local', `\nSSL_KEY_FILE=${keyFilePath}`);
  }
}

const apiUrl = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
  ? process.env.ASPNETCORE_URLS.split(';')[0]
  : 'http://localhost:59328';

export default defineConfig({
  plugins: [vue()],
  esbuild: {
    jsxFactory: 'h',
    jsxInject: "import {h} from 'vue';",
  },
  server: {
    port: 44489,
    https: {
      cert: certFilePath,
      key: keyFilePath,
    },
    proxy: {
      '^/api/.*': {
        target: apiUrl,
        changeOrigin: true,
        rewrite: path => path.replace(/^\/api/, apiUrl),
        headers: {
          Connection: 'keep-alive',
        },
        secure: false,
      },
    },
  },
  resolve: {
    alias: [
      {
        find: '@',
        replacement: resolve(__dirname, './src'),
      },
    ],
  },
});
