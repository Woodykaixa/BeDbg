<script setup lang="ts">
import { usePlugin } from '@/hooks/usePlugin';
import { NThing, NList, NLi, NCollapse, NCollapseItem, NTime } from 'naive-ui';

const pluginStore = usePlugin();
</script>

<template>
  <n-list>
    <template #header> <div style="font-weight: 600; padding-top: 0;">插件</div> </template>
    <n-li v-if="pluginStore.plugins.length === 0"> 暂无插件 </n-li>
    <n-li v-for="instance in pluginStore.plugins">
      <n-thing>
        <template #header
          >{{ instance.plugin.name }} &nbsp; 作者: {{ instance.plugin.author }} &nbsp; 版本:
          {{ instance.plugin.version }}</template
        >
        <template #description>{{ instance.plugin.description }}</template>
        <n-collapse>
          <n-collapse-item title="输出">
            <n-list>
              <n-li v-for="output in instance.output"> {{ output.data }} - 来自事件: {{ output.event }} </n-li>
            </n-list>
          </n-collapse-item>
          <n-collapse-item title="错误">
            <n-list>
              <n-li v-for="error in instance.error"> {{ error.data }} - 来自事件: {{ error.event }} </n-li>
            </n-list>
          </n-collapse-item>
        </n-collapse>
      </n-thing>
    </n-li>
  </n-list>
</template>
