<script setup lang="ts">
import { ref, effect, reactive } from 'vue';
import { NButton, NSpace, NList, NListItem, NThing, NScrollbar, NEmpty, NSpin, NTag } from 'naive-ui';
import { ProcessModel } from '../dto/process';
import { Api } from '../api';

function loadProcess() {
  const loading = ref(true);
  const data = ref([] as Array<ProcessModel>);
  effect(async () => {
    const processList = await Api.getProcessList();
    if (processList) {
      loading.value = false;
      data.value = processList;
    }
  });
  return {
    loading,
    data,
  };
}

const { loading: processLoading, data: processList } = loadProcess();
const selectedPid = ref(0);
const selectProcess = (pid: number) => {
  selectedPid.value = pid;
};
</script>

<template>
  <div class="page">
    <div style="display: flex; flex-direction: column; width: 60%">
      <h1 class="title">BeDbg</h1>
      <n-space style="width: fit-content; margin: auto">
        <n-button>启动</n-button>
        <n-button :disabled="selectedPid === 0">附加到进程</n-button>
      </n-space>
      <n-spin v-if="processLoading" spinning>
        <n-space justify="center" style="height: 200px" align="center"> 正在加载进程 </n-space>
      </n-spin>
      <n-list v-else>
        <template #header> 当前进程 </template>
        <n-scrollbar v-if="processList.length !== 0" style="height: 50vh; background-color: #f8f8f8; padding: 8px">
          <n-list-item
            v-for="process in processList"
            :key="process.id"
            class="process-item"
            @click="selectProcess(process.id)"
          >
            <n-thing>
              <template #header>
                <n-space>
                  <div>进程名称: {{ process.name }}</div>
                  <n-tag v-if="process.wow64" type="success" size="small">WOW64</n-tag>
                </n-space>
              </template>
              <template #description>
                <div>进程ID: {{ process.id }}</div>
                <div>进程标题: {{ process.title }}</div>
                <div>启动命令: {{ process.command }}</div>
              </template>
            </n-thing>
          </n-list-item>
        </n-scrollbar>
        <n-empty v-else title="没有获取到进程信息" style="width: 100%">
          <template #extra> 请尝试使用管理员模式运行 BeDbg </template>
        </n-empty>
      </n-list>
    </div>
  </div>
</template>

<style scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}

.page {
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.badge {
  border-radius: 2px;
  padding: 4px;
  font-size: 0.6rem;
}

.title {
  font-size: 3rem;
  text-align: center;
}

.process-item {
  cursor: pointer;
  padding: 4px;
}

.process-item:hover {
  background-color: rgba(99, 226, 183, 0.6);
  transition-timing-function: linear;
  transition: 0.2s;
}
</style>
