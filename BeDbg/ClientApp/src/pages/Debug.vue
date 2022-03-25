<template>
  <div class="menu">
    <n-button text @click="showDrawer = true">地址空间</n-button>
  </div>
  <n-drawer placement="right" v-model:show="showDrawer" width="800">
    <n-drawer-content closable>
      <n-collapse>
        <n-collapse-item title="地址空间">
          <n-scrollbar style="height: 50vh">
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
          </n-scrollbar>
        </n-collapse-item>
        <n-collapse-item title="符号">
          <n-scrollbar style="height: 50vh">
            <div class="symbol-layout">
              <div class="name">模块名称</div>
              <div class="entry">入口地址</div>
              <div class="size">模块大小</div>
              <div class="base">装载地址</div>
            </div>
            <code v-for="m in process.modules" class="symbol-layout">
              <div class="name">{{ m.name }}</div>
              <div class="entry">{{ formatNumberHex(m.entry) }}</div>
              <div class="size">{{ formatNumberHex(m.size) }}</div>
              <div class="base">{{ formatNumberHex(m.base) }}</div>
            </code>
          </n-scrollbar>
        </n-collapse-item>
      </n-collapse>
      <template #footer>
        <n-button @click="stopDebug">停止调试</n-button>
      </template>
    </n-drawer-content>
  </n-drawer>
  <div class="debug-container">
    <n-list class="dis-asm-box" bordered>
      <template #header> 反汇编 </template>
      <n-scrollbar>
        <n-li v-for="i in instr" class="dis-asm-instr">
          <div class="address">{{ formatNumberHex(i.address) }}</div>
          <n-code :code="i.text" language="x86asm" />
        </n-li>
      </n-scrollbar>
    </n-list>
    <div class="data-panel">
      <n-card class="register-box" title="寄存器" :content-style="{ display: 'flex', 'flex-wrap': 'wrap' }">
        <div v-for="reg in fakeReg" class="register">
          <code class="name">{{ reg.name }}</code>
          <code class="value">{{ formatNumberHex(reg.value) }}</code>
        </div>
      </n-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { effect, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  useNotification,
  NList,
  NLi,
  NScrollbar,
  NDrawer,
  NDrawerContent,
  NButton,
  NCollapse,
  NCollapseItem,
  NCode,
  NCard,
} from 'naive-ui';
import { Api } from '@/api';
import { ProcessModule } from '@/dto/process';

const showDrawer = ref(false);
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

const instr: Instruction[] = [];
for (let i = 0; i < 20; i++) {
  instr.push(
    ...[
      {
        address: 0,
        text: 'mov eax, ebx',
      },
      {
        address: 4,
        text: 'lea ebx, 8[eip]',
      },
      {
        address: 6,
        text: 'push eax',
      },
      {
        address: 7,
        text: 'push ebx',
      },
      {
        address: 11,
        text: 'call SomeFunction',
      },
      {
        address: 15,
        text: 'xor eax, eax',
      },
    ]
  );
}
const router = useRouter();
const notification = useNotification();
const process = reactive({ id: 0, handle: 0, attachTime: new Date(), modules: [] as ProcessModule[] });
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
      const { data, ok } = await Api.DebuggingProcess.listModules(process.id);
      if (ok) {
        process.modules = data.sort((a, b) => a.base - b.base);
      } else {
        notification.error({
          title: '无法获取进程模块',
          description: data.error,
          content: data.message,
        });
        router.push('/');
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
  height: 92vh;
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

.symbol-layout {
  display: flex;
  width: 100%;
}

.symbol-layout .entry,
.symbol-layout .base,
.symbol-layout .size,
.symbol-layout .name {
  width: 25%;
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
