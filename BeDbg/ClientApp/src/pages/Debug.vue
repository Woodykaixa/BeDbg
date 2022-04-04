<script setup lang="ts">
import { effect, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  useNotification,
  NList,
  NLi,
  NScrollbar,
  NCollapse,
  NCollapseItem,
  NCode,
  NLayout,
  NLayoutSider,
  NLayoutContent,
  NButton,
} from 'naive-ui';
import { Api } from '@/api';
import { ProcessModule, ProcessMemoryPage } from '@/dto/process';
import { ErrorResponse } from '@/dto/error';

const fakeReg = [
  { name: 'RAX', value: 0x34 },
  { name: 'RBX', value: 0 },
  { name: 'RCX', value: 0x00000096892ffa40 },
  { name: 'RDX', value: 0xa },
  { name: 'RBP', value: 0 },
  { name: 'RSP', value: 0x00000096892ffa18 },
  { name: 'RSI', value: 0 },
  { name: 'RDI', value: 0 },
  { name: 'R8', value: 0 },
  { name: 'R9', value: 0 },
  { name: 'R10', value: 0 },
  { name: 'R11', value: 0x0000000000000246 },
  { name: 'R12', value: 0 },
  { name: 'R13', value: 0 },
  { name: 'R14', value: 0 },
  { name: 'R15', value: 0 },
  { name: 'RIP', value: 0x00007ffeba6ed3f4 },
];
type Instruction = {
  address: number;
  text: string;
};

const formatNumberHex = (num: number) => {
  const comb = '0000000000000000' + num.toString(16);
  return '0x' + comb.slice(-16);
};

const formatProtectionFlag = (flag: ProcessMemoryPage['flags']) => {
  return `${flag.execute ? 'E' : '-'}${flag.read ? 'R' : '-'}${flag.write ? 'W' : '-'}${flag.copy ? 'C' : '-'}${
    flag.guard ? 'G' : '-'
  }`;
};

const debugData = reactive({
  instr: [] as Instruction[],
});

const router = useRouter();
const notification = useNotification();
const process = reactive({
  id: 0,
  handle: 0,
  attachTime: new Date(),
  modules: [] as ProcessModule[],
  pages: [] as (ProcessMemoryPage & { data: string })[],
});
async function CheckDebugging() {
  const { data, ok } = await Api.DebuggingProcess.list();
  if (ok) {
    if (data.length === 0) {
      notification.error({
        title: '未指定调试进程',
        content: '请附加或创建一个进程来调试',
      });
      router.push('/');
      return;
    }
    process.id = data[0].id;
    process.attachTime = data[0].attachTime;
    process.handle = data[0].handle;
    {
      const { data: modules, ok: moduleOk } = await Api.DebuggingProcess.listModules(process.id);
      const { data: pages, ok: pageOk } = await Api.DebuggingProcess.listPages(process.id);

      if (!moduleOk || !pageOk) {
        const err = (moduleOk ? pages : modules) as ErrorResponse;
        notification.error({
          title: '无法获取进程模块',
          description: err.error,
          content: err.message,
        });
        router.push('/');
        return;
      }
      const map = new Map<number, ProcessModule>();
      modules.forEach(m => {
        map.set(m.base, m);
      });
      process.modules = modules.sort((a, b) => a.base - b.base);
      process.pages = pages
        .sort((a, b) => a.baseAddress - b.baseAddress)
        .map(m => {
          const module = map.get(m.baseAddress);
          if (module) {
            return {
              ...m,
              data: module.name,
            };
          }
          return {
            ...m,
            data: '',
          };
        });
      const { ok, data } = await Api.DebuggingProcess.disassemble(process.id);
      if (ok) {
        debugData.instr = data.map(i => ({
          address: i.ip,
          text: i.text,
        }));
      }
    }
  } else {
    notification.error({
      title: '无法获取调试中进程',
      description: data.error,
      content: data.message,
    });
    router.push('/');
    return;
  }
}

onMounted(() => {
  CheckDebugging();
});

const stopDebug = async () => {
  sessionStorage.removeItem('debugPid');
  const { ok, data } = await Api.DebuggingProcess.detachProcess(process.id);
  if (ok) {
    router.push('/');
  } else {
    notification.error({
      title: '退出调试失败',
      description: data.error,
      content: data.message,
    });
  }
};

effect(async () => {
  const resp = await Api.readProcessMemory({ pid: process.id, address: 0, size: 0 })!;
  console.log(resp, typeof resp);
});
</script>

<template>
  <n-layout has-sider>
    <n-layout-sider
      collapse-mode="width"
      :collapsed-width="0"
      :width="800"
      show-trigger="arrow-circle"
      content-style="padding: 24px; height:100vh;"
      bordered
      :native-scrollbar="false"
    >
      <n-scrollbar>
        <n-collapse>
          <n-collapse-item title="载入模块">
            <div class="memory-layout">
              <div class="name">模块名称</div>
              <div class="entry">入口地址</div>
              <div class="size">模块大小</div>
              <div class="base">装载地址</div>
            </div>
            <code v-for="m in process.modules" class="memory-layout">
              <div class="name">{{ m.name }}</div>
              <div class="entry">{{ formatNumberHex(m.entry) }}</div>
              <div class="size">{{ formatNumberHex(m.size) }}</div>
              <div class="base">{{ formatNumberHex(m.base) }}</div>
            </code>
          </n-collapse-item>
          <n-collapse-item title="进程内存">
            <div class="memory-page-layout">
              <div class="address">地址</div>
              <div class="address">大小</div>
              <div class="flags">页面信息</div>
              <div class="flags">类型</div>
              <div class="flags">页面保护</div>
              <div class="flags">初始保护</div>
            </div>
            <code v-for="m in process.pages" class="memory-page-layout">
              <div class="address">{{ formatNumberHex(m.baseAddress) }}</div>
              <div class="address">{{ formatNumberHex(m.size) }}</div>
              <div class="flags">{{ m.data }}</div>
              <div class="flags">{{ m.type === 0x1000000 ? 'IMAGE' : m.type === 0x40000 ? 'MAPPED' : 'PRIVATE' }}</div>
              <div class="flags">
                {{ formatProtectionFlag(m.flags) }}
              </div>
              <div class="flags">
                {{ formatProtectionFlag(m.initialFlags) }}
              </div>
            </code>
          </n-collapse-item>
          <n-collapse-item title="寄存器">
            <div class="register-box" :style="{ display: 'flex', 'flex-wrap': 'wrap' }">
              <div v-for="reg in fakeReg" class="register">
                <code class="name">{{ reg.name }}</code>
                <code class="value">{{ formatNumberHex(reg.value) }}</code>
              </div>
            </div>
          </n-collapse-item>
          <n-collapse-item title="其他功能">
            <n-button @click="stopDebug"> 结束调试 </n-button>
          </n-collapse-item>
        </n-collapse>
      </n-scrollbar>
    </n-layout-sider>
    <n-layout-content>
      <div class="debug-container">
        <n-list class="dis-asm-box" bordered>
          <template #header> 反汇编 </template>
          <n-scrollbar>
            <n-li v-for="i in debugData.instr" class="dis-asm-instr">
              <div class="address">{{ formatNumberHex(i.address) }}</div>
              <n-code :code="i.text" language="x86asm" />
            </n-li>
          </n-scrollbar>
        </n-list>
      </div>
    </n-layout-content>
  </n-layout>
</template>

<style scoped>
.debug-page {
  box-sizing: border-box;
  width: 100vw;
  height: 100vh;
}

.menu {
  box-sizing: border-box;
  height: 8vh;
  width: 100vw;
  display: flex;
  padding: 12px;
}

.menu button {
  width: 10%;
}
.debug-container {
  box-sizing: border-box;
  width: 80vw;
  height: 100vh;
  display: flex;
  padding: 16px;
  margin: auto;
}

.dis-asm-box {
  width: 70%;
  display: flex;
  flex-direction: column;
  font-family: Consolas;
  margin-block-start: 0;
  margin-block-end: 0;
}

.dis-asm-instr {
  width: 100%;
  display: flex;
}

.dis-asm-instr .address {
  width: 70%;
  min-width: 150px;
  max-width: 200px;
  text-align: center;
}

.memory-layout {
  display: flex;
  width: 100%;
}

.memory-layout .entry,
.memory-layout .base,
.memory-layout .size,
.memory-layout .name {
  width: 25%;
}

.memory-page-layout {
  display: flex;
  width: 100%;
  flex-wrap: nowrap;
}

.memory-page-layout .address {
  width: 20%;
}

.memory-page-layout .flags {
  width: 15%;
}

.register-box {
  margin-left: 4px;
}

.register-box .register {
  box-sizing: border-box;
  padding: 4px;
  display: flex;
}

.register-box .register .name {
  width: 30px;
  margin: 0 4px;
}

.register-box .register .value {
}

.data-panel {
  width: 30%;
  display: flex;
  flex-direction: column;
}
</style>
