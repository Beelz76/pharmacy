<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Профиль администратора</h1>
    <div class="bg-white rounded-lg shadow p-6 max-w-xl space-y-2">
      <p><b>Email:</b> {{ account?.email }}</p>
      <p><b>Имя:</b> {{ fullName }}</p>
      <p><b>Телефон:</b> {{ account?.phone || '—' }}</p>
    </div>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useAccountStore } from '/src/stores/AccountStore'

const accountStore = useAccountStore()
const { account, fullName } = storeToRefs(accountStore)

onMounted(() => accountStore.fetchProfile())
</script>