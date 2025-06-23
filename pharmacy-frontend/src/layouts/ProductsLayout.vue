<template>
  <div class="max-w-7xl mx-auto px-2 py-6 md:py-10">
    <div class="grid md:grid-cols-[260px_1fr] gap-8">
      <ProductsSidebar
        :selected-filters="filters"
        @update:filters="onFiltersUpdate"
      />
      <router-view :filters="filters" @update:filters="onFiltersUpdate" />
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import ProductsSidebar from "../components/sidebars/ProductsSidebar.vue";

const filters = ref({
  isAvailable: true,
  manufacturerIds: [],
  countries: [],
  propertyFilters: {},
  categoryIds: [],
});

const route = useRoute();
const router = useRouter();

watch(
  () => [route.query.manufacturerIds, route.query.countries],
  ([manIds, countries]) => {
    const newManIds = manIds
      ? Array.isArray(manIds)
        ? manIds.map((m) => Number(m))
        : [Number(manIds)]
      : [];
    const newCountries = countries
      ? Array.isArray(countries)
        ? countries
        : [countries]
      : [];

    if (
      JSON.stringify(newManIds) !==
      JSON.stringify(filters.value.manufacturerIds)
    ) {
      filters.value.manufacturerIds = newManIds;
    }

    if (
      JSON.stringify(newCountries) !== JSON.stringify(filters.value.countries)
    ) {
      filters.value.countries = newCountries;
    }
  },
  { immediate: true }
);

const onFiltersUpdate = (newFilters) => {
  filters.value = { ...newFilters };
  const query = {
    ...(filters.value.manufacturerIds.length && {
      manufacturerIds: filters.value.manufacturerIds,
    }),
    ...(filters.value.countries.length && {
      countries: filters.value.countries,
    }),
  };
  const newQuery = { ...route.query, ...query };
  if (!filters.value.manufacturerIds.length) delete newQuery.manufacturerIds;
  if (!filters.value.countries.length) delete newQuery.countries;
  router.replace({ query: newQuery });
};
</script>
