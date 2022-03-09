<script setup lang="ts">
import { ref, effect } from 'vue';
import HelloWorld from './components/HelloWorld.vue';
import { Test } from './components/test';
import { NButton } from 'naive-ui';
const message = ref('Hello world');
effect(async () => {
  const apiTest = await fetch('/api/test');
  setTimeout(async () => {
    message.value = '/api/test response: ' + (await apiTest.text());
  }, 1000);
});
</script>

<template>
  <img alt="Vue logo" src="./assets/logo.png" />
  <HelloWorld :msg="message" />
  <Test />
  <n-button>naive ui</n-button>
</template>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
