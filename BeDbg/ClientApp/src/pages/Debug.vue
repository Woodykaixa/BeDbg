<template>DebugView: PID: {{ route.params.pid }}</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
onMounted(() => {
  console.log('route', route.params);
});
</script>
