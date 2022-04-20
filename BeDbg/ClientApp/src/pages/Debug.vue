<script setup lang="ts">
import { effect, onMounted, provide, reactive, ref } from 'vue';
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
import { DebuggerEventSource } from '@/util/debuggerEventSource';
import { DataFormatter } from '@/util/formatter';
import DebugViewSider from '@/components/DebugViewSider.vue';
import { provideDebuggerEventSource } from '@/hooks/useDebuggerEvent';

const debuggerEvent = new DebuggerEventSource('/api/debugger/0/event');
debuggerEvent.addEventListener('notFound', () => {
  debuggerEvent.close(); // If debugger not present, close the event source
});

provideDebuggerEventSource(debuggerEvent);

type Instruction = {
  address: number;
  text: string;
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

async function InitializeDebugger() {
  debuggerEvent.addEventListener('createProcess', e => {
    console.log(e);
  });

  debuggerEvent.addEventListener('exitProgram', stopDebug);

  debuggerEvent.addEventListener('programReady', async () => {
    debuggerEvent.addEventListener('exception', async exception => {
      console.log(exception);
      const { ok, data } = await Api.DebuggingProcess.disassemble(process.id, exception.exceptionAddress + 1);
      if (ok) {
        debugData.instr = data.map(i => ({
          address: i.ip,
          text: i.text,
        }));
      }
    });
  });
}

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
  CheckDebugging(); // If debugger is not present, we will redirect to home page.
  InitializeDebugger(); // So we don't need to check return value here.
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
      <debug-view-sider :pid="process.id" v-if="process.id !== 0" />
    </n-layout-sider>
    <n-layout-content>
      <div class="debug-container">
        <n-list class="dis-asm-box" bordered>
          <template #header> 反汇编 </template>
          <n-scrollbar>
            <n-li v-for="i in debugData.instr" class="dis-asm-instr">
              <div class="address">{{ DataFormatter.formatNumberHex(i.address) }}</div>
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
