<script setup lang="ts">
import { ref, effect, onMounted, watch } from 'vue';
import {
  NList,
  NListItem,
  NThing,
  NScrollbar,
  NEmpty,
  NSpin,
  NIcon,
  NButton,
  NBreadcrumb,
  NBreadcrumbItem,
  NInput,
} from 'naive-ui';
import { Api } from '@/api';
import { FileModel } from '@/dto/fs';
import { FileOutlined, FolderOutlined } from '@vicons/antd';

const loading = ref(true);
const files = ref([] as Array<FileModel>);
const cwd = ref('');
const targetFile = ref('');
const folders = ref([] as string[]);
const requestFileList = (dir?: string) => {
  effect(async () => {
    loading.value = true;
    targetFile.value = '';
    const fileList = await Api.getFileList(dir);
    if (fileList) {
      loading.value = false;
      files.value = fileList.files;
      cwd.value = fileList.path;
    }
  });
};

onMounted(requestFileList);

const clickBreadcrumb = (index: number) => {
  const path = folders.value.slice(0, index + 1).join('\\');
  console.log('request path', path);
  requestFileList(path);
};

watch(
  () => cwd.value,
  () => {
    folders.value = cwd.value.split('\\');
    console.log('folders', folders.value);
  }
);
</script>

<template>
  <n-spin :show="loading">
    <n-list class="panel">
      <template #header>
        <n-breadcrumb separator="/">
          <n-breadcrumb-item v-for="(path, index) in folders" :key="index">
            <n-button text @click="clickBreadcrumb(index)">{{ path }}</n-button>
          </n-breadcrumb-item>
        </n-breadcrumb>
      </template>
      <div style="display: flex; width: 100%; margin-bottom: 8px">
        <n-input placeholder="启动命令" style="flex: 1%" />
        <n-button :disabled="targetFile === ''">启动</n-button>
      </div>
      <n-scrollbar v-if="files.length !== 0" style="height: 50vh; padding: 8px; background-color: rgb(26, 26, 26)">
        <n-list-item
          v-for="file in files"
          :key="file.name"
          class="process-item"
          :class="targetFile === file.path ? ['process-item', 'file-item-selected'] : ['process-item']"
        >
          <n-thing @click="file.type === 'folder' ? requestFileList(file.path) : (targetFile = file.path)">
            <template #header>
              {{ file.name }}
            </template>
            <template #description>
              {{ file.path }}
            </template>
            <template #avatar>
              <n-icon size="24">
                <file-outlined v-if="file.type === 'file'" />
                <folder-outlined v-else-if="file.type === 'folder'" />
              </n-icon>
            </template>
          </n-thing>
        </n-list-item>
      </n-scrollbar>
      <n-empty v-else title="空文件夹" class="panel-empty">
        <template #extra>空文件夹</template>
      </n-empty>
    </n-list>
  </n-spin>
</template>

<style scoped>
.panel {
  width: 100%;
  height: 60vh;
  position: relative;
}

.panel-empty {
  width: 100%;
  height: 100%;
  position: absolute;
  left: 50%;
  top: 50%;
  transform: translate(-50%, -50%);
  justify-content: center;
  pointer-events: none;
}
.process-item {
  cursor: pointer;
  padding: 4px;
}

.file-item-selected {
  background-color: rgba(71, 223, 172, 0.6);
}
.process-item:hover {
  background-color: rgba(99, 226, 183, 0.6);
  transition-timing-function: linear;
  transition: 0.2s;
}
</style>
