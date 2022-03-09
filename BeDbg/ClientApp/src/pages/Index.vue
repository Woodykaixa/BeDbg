<script setup lang="ts">
import { ref, effect } from 'vue';
import { NButton, NSpace, NList, NListItem, NThing, NScrollbar, NEmpty } from 'naive-ui';
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
</script>

<template>
  <n-space justify="center" class="full" align="center">
    <n-space vertical justify="center" class="container" align="center" item-style="width: 100%">
      <h1 class="title">BeDbg</h1>
      <n-space style="width: fit-content; margin: auto">
        <NButton>启动</NButton>
        <NButton>附加到进程</NButton>
      </n-space>
      <n-scrollbar style="height: 50vh; background-color: #f8f8f8; padding: 8px">
        <n-list>
          <template #header> 当前进程 </template>
          <n-list-item
            v-if="processList.length !== 0"
            v-for="process in processList"
            :key="process.id"
            class="process-item"
          >
            <n-thing style="width: 100%">
              <template #header> 进程名称: {{ process }} </template>
              <template #description>
                <div>进程ID: {{ process.id }}</div>
                <div>进程标题: {{ process.title }}</div>
              </template>
            </n-thing>
          </n-list-item>
          <n-empty v-else title="没有获取到进程信息" style="width: 100%">
            <template #extra> 请尝试使用管理员模式运行 BeDbg </template>
          </n-empty>
        </n-list>
      </n-scrollbar>
    </n-space>
  </n-space>
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

.full {
  width: 100vw;
  height: 100vh;
}

.container {
  min-width: 600px;
}

.title {
  font-size: 3rem;
  text-align: center;
}

.process-item {
  cursor: pointer;
}

.process-item:hover {
  background-color: #63e2b7;
  transition-timing-function: linear;
  transition: 0.2s;
}
</style>
