<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Заказы</h1>

    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <el-form-item label-width="0">
          <el-input v-model="filters.number" placeholder="Номер" size="large" class="!w-40" />
        </el-form-item>
        <el-form-item label-width="0">
          <el-input v-model="filters.userEmail" placeholder="Email" size="large" class="!w-48" />
        </el-form-item>
        <el-form-item label-width="0">
          <el-input v-model="filters.userFullName" placeholder="ФИО" size="large" class="!w-52" />
        </el-form-item>
        <el-form-item label="Аптека">
          <el-select
            v-model="selectedPharmacyName"
            placeholder="Все"
            clearable
            filterable
            remote
            reserve-keyword
            :remote-method="searchPharmacyNames"
            :loading="loadingPharmacies"
            class="!w-52"
          >
            <el-option v-for="n in pharmacyNames" :key="n" :label="n" :value="n" />
          </el-select>
        </el-form-item>
        <el-form-item label-width="0">
          <el-select
            v-model="filters.pharmacyId"
            placeholder="Адрес"
            clearable
            filterable
            remote
            reserve-keyword
            :remote-method="searchPharmacyAddresses"
            :loading="loadingPharmacies"
            class="!w-60"
          >
            <el-option v-for="p in pharmacyAddresses" :key="p.id" :label="p.address" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="Статус">
          <el-select v-model="filters.status" placeholder="Все" clearable class="!w-40">
            <el-option v-for="s in statuses" :key="s.id" :label="s.description" :value="s.name" />
          </el-select>
        </el-form-item>
        <el-form-item label="Период">
          <el-date-picker v-model="filters.dateRange" type="daterange" range-separator="-" start-placeholder="От" end-placeholder="До" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" plain @click="fetch">Поиск</el-button>
          <el-button @click="resetFilters">Сбросить</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm">
          <tr>
            <th class="px-6 py-5 font-semibold">Номер</th>
            <th class="px-6 py-5 font-semibold">Дата</th>
            <th class="px-6 py-5 font-semibold">Сумма</th>
            <th class="px-6 py-5 font-semibold">Статус</th>
            <th class="px-6 py-5 font-semibold">Аптека</th>
            <th class="px-6 py-5 font-semibold">Адрес</th>
            <th class="px-6 py-5 font-semibold text-right"><span class="sr-only">Детали</span></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="o in orders" :key="o.id" class="hover:bg-secondary-50 cursor-pointer" @click="goDetails(o.id)">
            <td class="px-6 py-4 whitespace-nowrap">{{ o.number }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ formatDate(o.createdAt) }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.totalPrice.toFixed(2) }} ₽</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.status }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.pharmacyName }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.pharmacyAddress }}</td>
            <td class="px-6 py-4 text-right text-gray-400"><i class="fas fa-chevron-right"></i></td>
          </tr>
          <tr v-if="!loading && orders.length === 0">
            <td colspan="7" class="text-center py-6 text-gray-500">Заказы не найдены</td>
          </tr>
          <tr v-if="loading">
            <td colspan="7" class="text-center py-6 text-gray-500">Загрузка...</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="totalCount > pageSize" class="flex justify-center mt-6">
      <el-pagination layout="prev, pager, next" :total="totalCount" :page-size="pageSize" v-model:current-page="pageNumber" />
    </div>
  </div>
</template>

<script setup>
import { reactive, watch, ref } from 'vue'
import { useOrders } from '/src/composables/useOrders'
import { useRouter } from 'vue-router'
import { getPharmacies } from '/src/services/PharmacyService'
import { getOrderStatuses } from '/src/services/OrderService'
import formatAddress from '/src/utils/formatAddress'

const router = useRouter()
const filters = reactive({
  number: '',
  userEmail: '',
  userFullName: '',
  pharmacyName: '',
  pharmacyId: null,
  status: null,
  dateRange: []
})

const { orders, totalCount, pageNumber, pageSize, loading, fetchOrders } = useOrders()

const pharmacyNames = ref([])
const pharmacyAddresses = ref([])
const selectedPharmacyName = ref(null)
const statuses = ref([])
const loadingPharmacies = ref(false)

const searchPharmacyNames = async (query = '') => {
  loadingPharmacies.value = true
  try {
    const data = await getPharmacies({ search: query })
    const names = Array.from(new Set(data.items.map(p => p.name)))
    pharmacyNames.value = names
  } finally {
    loadingPharmacies.value = false
  }
}

const searchPharmacyAddresses = async (query = '') => {
  if (!selectedPharmacyName.value) {
    pharmacyAddresses.value = []
    return
  }
  loadingPharmacies.value = true
  try {
    const data = await getPharmacies({ search: selectedPharmacyName.value })
    let list = data.items.filter(p => p.name === selectedPharmacyName.value)
    if (query) {
      const q = query.toLowerCase()
      list = list.filter(p => formatAddress(p.address).toLowerCase().includes(q))
    }
    pharmacyAddresses.value = list.map(p => ({ id: p.id, address: formatAddress(p.address) }))
  } finally {
    loadingPharmacies.value = false
  }
}

const loadStatuses = async () => {
  statuses.value = await getOrderStatuses()
}

const fetch = () => {
  const payload = {
    number: filters.number || null,
    userEmail: filters.userEmail || null,
    userFullName: filters.userFullName || null,
    pharmacyName: filters.pharmacyName || null,
    pharmacyId: filters.pharmacyId || null,
    status: filters.status || null,
    fromDate: filters.dateRange?.[0] || null,
    toDate: filters.dateRange?.[1] || null
  }
  fetchOrders({ page: pageNumber.value, size: pageSize.value, filters: payload })
}

const resetFilters = () => {
  filters.number = ''
  filters.userEmail = ''
  filters.userFullName = '',
  filters.pharmacyName = '',
  filters.pharmacyId = null
  filters.status = null
  filters.dateRange = []
  pageNumber.value = 1
  fetch()
}

watch(pageNumber, fetch)

const formatDate = (iso) => {
  return new Date(iso).toLocaleString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}

const goDetails = (id) => {
  router.push({ name: 'AdminOrderDetails', params: { id } })
}

watch(selectedPharmacyName, () => {
  filters.pharmacyId = null
  filters.pharmacyName = selectedPharmacyName.value || ''
  searchPharmacyAddresses()
})

watch(() => filters.pharmacyName, () => {
  pageNumber.value = 1
})

loadStatuses()
searchPharmacyNames()
fetch()
</script>