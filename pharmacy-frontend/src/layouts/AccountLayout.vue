<template>
  <div class="flex flex-col md:flex-row max-w-7xl mx-auto px-4 py-6 gap-6">
    <AccountSidebar />
    <main class="flex-1 bg-slate-50 p-4 rounded shadow">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AccountSidebar from '../components/account/AccountSidebar.vue'
</script>
