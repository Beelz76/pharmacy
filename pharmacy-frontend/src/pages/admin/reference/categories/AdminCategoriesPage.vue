<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Категории</h1>
    <div class="bg-white rounded-lg shadow p-6 flex" v-if="loaded">
      <div class="w-1/3 pr-6 border-r">
        <div class="mb-4 text-right">
          <el-button size="small" type="primary" @click="createCategory"
            >Создать</el-button
          >
        </div>
        <el-tree
          :data="categories"
          node-key="id"
          :default-expand-all="true"
          @node-click="selectCategory"
        />
      </div>

      <div class="flex-1 pl-6" v-if="selected">
        <h2 class="text-xl font-semibold mb-4">{{ selected.name }}</h2>
        <div class="mb-4">
          <el-button size="small" type="primary" @click="openEdit"
            >Редактировать</el-button
          >
          <el-button size="small" type="danger" @click="removeCategory"
            >Удалить</el-button
          >
        </div>
        <div>
          <p class="text-gray-700 mb-2">{{ selected.description }}</p>
          <div v-if="selected.fields?.length">
            <h3 class="font-semibold mb-2">Поля</h3>
            <ul class="list-disc pl-5 text-sm space-y-1">
              <li v-for="f in selected.fields" :key="f.id">
                {{ f.label }} ({{ f.key }})
                <span v-if="f.isRequired" class="text-red-500">*</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
      <div class="flex-1 pl-6" v-else>
        <p class="text-gray-500">Выберите категорию</p>
      </div>

      <el-dialog v-model="editVisible" title="Категория">
        <el-form label-width="120px">
          <el-form-item label="Название">
            <el-input v-model="form.name" />
          </el-form-item>
          <el-form-item label="Описание">
            <el-input v-model="form.description" />
          </el-form-item>
          <el-form-item label="Родитель">
            <el-select v-model="form.parentCategoryId" clearable>
              <el-option :value="null" label="Нет" />
              <el-option
                v-for="c in flatCategories"
                :key="c.id"
                :value="c.id"
                :label="c.name"
              />
            </el-select>
          </el-form-item>
        </el-form>
        <template #footer>
          <span class="dialog-footer">
            <el-button @click="editVisible = false">Отмена</el-button>
            <el-button type="primary" @click="saveCategory"
              >Сохранить</el-button
            >
          </span>
        </template>
      </el-dialog>
    </div>
    <div v-else class="text-center py-10">Загрузка...</div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import {
  getAllCategories,
  getCategoryById,
  createCategoryApi,
  updateCategory,
  deleteCategory,
} from "/src/services/CategoryService";
import { ElMessage, ElMessageBox } from "element-plus";

const categories = ref([]);
const flatCategories = ref([]);
const loaded = ref(false);
const selectedId = ref(null);
const selected = ref(null);
const editVisible = ref(false);
const form = reactive({ name: "", description: "", parentCategoryId: null });

function flatten(list, arr = []) {
  for (const c of list) {
    arr.push({ id: c.id, name: c.name });
    if (c.subcategories?.length) flatten(c.subcategories, arr);
  }
  return arr;
}

async function load() {
  loaded.value = false;
  try {
    categories.value = await getAllCategories();
    flatCategories.value = flatten(categories.value);
  } finally {
    loaded.value = true;
  }
}

function selectCategory(node) {
  selectedId.value = node.id;
  selected.value = node;
  form.name = node.name;
  form.description = node.description;
  form.parentCategoryId = node.parentCategoryId || null;
}

function createCategory() {
  selected.value = null;
  form.name = "";
  form.description = "";
  form.parentCategoryId = null;
  editVisible.value = true;
}

function openEdit() {
  if (!selected.value) return;
  editVisible.value = true;
}

async function saveCategory() {
  try {
    if (selected.value) {
      await updateCategory(selectedId.value, {
        name: form.name,
        description: form.description,
        parentCategoryId: form.parentCategoryId,
      });
      ElMessage.success("Категория обновлена");
    } else {
      const newCat = await createCategoryApi({
        name: form.name,
        description: form.description,
        parentCategoryId: form.parentCategoryId,
        fields: [],
      });
      ElMessage.success("Категория создана");
      selectedId.value = newCat.id;
    }
    editVisible.value = false;
    await load();
  } catch (e) {
    ElMessage.error("Ошибка сохранения");
  }
}

async function removeCategory() {
  try {
    await ElMessageBox.confirm(
      "Вы уверены, что хотите удалить категорию?",
      "Подтверждение",
      {
        confirmButtonText: "Удалить",
        cancelButtonText: "Отмена",
        type: "warning",
      }
    );
  } catch {
    return;
  }
  try {
    await deleteCategory(selectedId.value);
    ElMessage.success("Категория удалена");
    selected.value = null;
    await load();
  } catch (e) {
    ElMessage.error("Ошибка удаления");
  }
}

onMounted(load);
</script>
