<template>
  <aside class="bg-white border rounded-xl shadow-sm p-4 w-full space-y-6">
    <!-- Текущая категория -->
    <div v-if="categoryTitle" class="space-y-1">
      <div class="text-sm text-gray-500 uppercase tracking-wide">Категория</div>
      <div class="text-base font-semibold text-gray-800">
        {{ categoryTitle }}
      </div>
    </div>
    <!-- Заголовок -->
    <h2 class="text-lg font-semibold text-gray-800">Фильтры</h2>

    <!-- В наличии -->
    <div>
      <el-checkbox
        v-model="inStockOnly"
        label="Только в наличии"
        class="!text-sm"
      />
    </div>

    <!-- Фильтры -->
    <el-collapse class="mt-4" accordion>
      <el-collapse-item v-if="manufacturers.length" name="manufacturers">
        <template #title>
          <span class="text-sm font-medium text-gray-700">Производитель</span>
        </template>
        <el-checkbox-group v-model="localFilters.manufacturerIds">
          <el-checkbox
            v-for="m in manufacturers"
            :key="m.id"
            :label="m.id"
            class="block mb-1 text-sm"
          >
            {{ m.name }}
          </el-checkbox>
        </el-checkbox-group>
      </el-collapse-item>

      <el-collapse-item v-if="countries.length" name="countries">
        <template #title>
          <span class="text-sm font-medium text-gray-700">Страна</span>
        </template>
        <el-checkbox-group v-model="localFilters.countries">
          <el-checkbox
            v-for="c in countries"
            :key="c"
            :label="c"
            class="block mb-1 text-sm"
          >
            {{ c }}
          </el-checkbox>
        </el-checkbox-group>
      </el-collapse-item>

      <el-collapse-item
        v-for="filter in propertyFilters"
        :key="filter.key"
        :name="filter.key"
      >
        <template #title>
          <span class="text-sm font-medium text-gray-700">{{
            filter.label
          }}</span>
        </template>

        <div
          class="max-h-[200px] overflow-y-auto pr-1"
          :class="{ 'custom-scroll': true }"
        >
          <el-checkbox-group v-model="localFilters.propertyFilters[filter.key]">
            <el-checkbox
              v-for="val in filter.values"
              :key="val"
              :label="val"
              class="block mb-1 text-sm"
            >
              {{ val }}
            </el-checkbox>
          </el-checkbox-group>
        </div>
      </el-collapse-item>
    </el-collapse>

    <!-- Кнопки -->
    <div class="flex flex-col gap-2 pt-4">
      <el-button type="primary" plain class="!h-10" @click="applyFilters">
        Применить
      </el-button>
      <el-button class="!h-10 !ml-0" @click="resetFilters">
        Сбросить
      </el-button>
    </div>
  </aside>
</template>

<script setup>
import { reactive, ref, watch, computed, onMounted } from "vue";
import { useCategoryStore } from "../../stores/CategoryStore";
import { useManufacturerStore } from "../../stores/ManufacturerStore";

const props = defineProps({
  selectedFilters: Object,
});
const emit = defineEmits(["update:filters"]);

const categoryStore = useCategoryStore();
const propertyFilters = computed(() => categoryStore.propertyFilters);

const localFilters = reactive({
  propertyFilters: {},
  manufacturerIds: [],
  countries: [],
});

const inStockOnly = ref(true);
const manufacturerStore = useManufacturerStore();
const manufacturers = computed(() => manufacturerStore.list);
const countries = computed(() => manufacturerStore.countries);

onMounted(() => {
  manufacturerStore.fetchManufacturers();
});

watch(
  () => props.selectedFilters,
  (newVal) => {
    inStockOnly.value = newVal?.isAvailable ?? true;
    localFilters.propertyFilters = JSON.parse(
      JSON.stringify(newVal?.propertyFilters ?? {})
    );
    localFilters.manufacturerIds = [...(newVal?.manufacturerIds ?? [])];
    localFilters.countries = [...(newVal?.countries ?? [])];
  },
  { immediate: true, deep: true }
);

watch(
  () => categoryStore.selectedCategoryId,
  () => {
    resetFilters();
  }
);

function applyFilters() {
  emit("update:filters", {
    isAvailable: inStockOnly.value ? true : null,
    manufacturerIds: [...localFilters.manufacturerIds],
    countries: [...localFilters.countries],
    propertyFilters: { ...localFilters.propertyFilters },
    categoryIds: categoryStore.selectedCategoryId
      ? [categoryStore.selectedCategoryId]
      : [],
  });
}

function resetFilters() {
  inStockOnly.value = true;
  localFilters.manufacturerIds = [];
  localFilters.countries = [];

  for (const key in localFilters.propertyFilters) {
    localFilters.propertyFilters[key] = [];
  }

  emit("update:filters", {
    isAvailable: true,
    manufacturerIds: [],
    countries: [],
    propertyFilters: {},
    categoryIds: categoryStore.selectedCategoryId
      ? [categoryStore.selectedCategoryId]
      : [],
  });
}

const categoryTitle = computed(() => {
  const current = categoryStore.selectedCategoryName;
  const parent = categoryStore.parentCategoryName;
  return parent ? `${parent} / ${current}` : current;
});
</script>

<style scoped>
.custom-scroll::-webkit-scrollbar {
  width: 6px;
}

.custom-scroll::-webkit-scrollbar-thumb {
  background-color: #cbd5e1; /* light slate */
  border-radius: 3px;
}
</style>
