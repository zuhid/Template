<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { numericTypeStore } from '../store';

const tableRef = ref(null);
const router = useRouter();

const columns = [
  { key: 'smallintType', label: 'Smallint' },
  { key: 'integerType', label: 'Integer' },
  { key: 'bigintType', label: 'Bigint' },
  { key: 'actions', label: 'Actions', sortable: false },
];

onMounted(() => tableRef.value?.reload());

async function refresh() {
  await numericTypeStore.forceRefresh();
  await tableRef.value?.reload();
}

async function remove(row) {
  if (!confirm(`Delete "${row.smallintType} / ${row.integerType} / ${row.bigintType}"?`)) return;
  const result = await numericTypeStore.delete(row.id);
  if (result !== undefined) await refresh();
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card id="numeric_types" title="Numeric Types" saveText="" cancelText="">
    <Table ref="tableRef" :store="numericTypeStore" :columns="columns" filterable>
      <template #toolbar>
        <Button id="add_numeric_type" label="Add" @click="router.push('/numeric-types/new')" />
      </template>
      <template #actions="{ row }">
        <div class="actions">
          <Button :id="'edit_' + row.id" label="Edit" @click="router.push(`/numeric-types/${row.id}/edit`)" />
          <Button :id="'delete_' + row.id" label="Delete" @click="remove(row)" />
        </div>
      </template>
    </Table>
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#numeric_types {
  min-width: 700px;
}

.actions {
  display: flex;
  gap: var(--gap-md);
}
</style>
