<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { dateTypeStore } from '../store';

const tableRef = ref(null);
const router = useRouter();

const columns = [
  { key: 'dateTimeWithoutTzType', label: 'DateTime (no TZ)' },
  { key: 'dateTimeType', label: 'DateTime' },
  { key: 'timeSpanType', label: 'TimeSpan' },
  { key: 'dateOnlyType', label: 'Date' },
  { key: 'timeOnlyType', label: 'Time' },
  { key: 'actions', label: 'Actions', sortable: false },
];

const display = (v) => (v ? String(v).replace('T', ' ') : '—');
const pad = (n) => String(n).padStart(2, '0');
const toLocalFromInstant = (v) => {
  if (!v) return '';
  const d = new Date(/(Z|[+-]\d\d:?\d\d)$/.test(v) ? v : v + 'Z');
  if (isNaN(d)) return '';
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
};
const displayInstant = (v) => (v ? toLocalFromInstant(v).replace('T', ' ') : '—');

onMounted(() => tableRef.value?.reload());

async function refresh() {
  await dateTypeStore.forceRefresh();
  await tableRef.value?.reload();
}

async function remove(row) {
  if (!confirm(`Delete "${row.id}"?`)) return;
  const result = await dateTypeStore.delete(row.id);
  if (result !== undefined) await refresh();
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card id="date_types" title="Date Types" saveText="" cancelText="">
    <Table ref="tableRef" :store="dateTypeStore" :columns="columns" filterable>
      <template #toolbar>
        <Button id="add_date_type" label="Add" @click="router.push('/date-types/new')" />
      </template>
      <template #dateTimeWithoutTzType="{ row }">{{ display(row.dateTimeWithoutTzType) }}</template>
      <template #dateTimeType="{ row }">{{ displayInstant(row.dateTimeType) }}</template>
      <template #actions="{ row }">
        <div class="actions">
          <Button :id="'edit_' + row.id" label="Edit" @click="router.push(`/date-types/${row.id}/edit`)" />
          <Button :id="'delete_' + row.id" label="Delete" @click="remove(row)" />
        </div>
      </template>
    </Table>
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#date_types {
  min-width: 900px;
}

.actions {
  display: flex;
  gap: var(--gap-md);
}
</style>
