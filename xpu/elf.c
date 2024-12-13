#pragma once
#include <stddef.h>
#include <stdint.h>
#include <string.h>
#include <stdio.h>
#include "dlib.h"

#define PACK(e) e

typedef struct elf_header elf_header_t;
typedef struct elf_phead elf_phead_t;
typedef struct elf_shead elf_shead_t;
typedef struct elf_sym32 elf_sym32_t;
typedef struct elf_dynamic elf_dynamic_t;


PACK(struct elf_header
{
    uint8_t sign[16];
    uint16_t type;
    uint16_t machine;
    uint32_t version;
    uint32_t entry;
    uint32_t ph_off;
    uint32_t sh_off;
    uint32_t flags;
    uint16_t eh_size;
    uint16_t ph_size;
    uint16_t ph_count;
    uint16_t sh_size;
    uint16_t sh_count;
    uint16_t sh_str_ndx;
});

PACK(struct elf_phead
{
    uint32_t type;
    uint32_t file_addr;
    uint32_t virt_addr;
    uint32_t phys_addr;
    uint32_t file_size;
    uint32_t virt_size;
    uint32_t flags;
    uint32_t align;
});

PACK(struct elf_shead
{
    uint32_t name_idx;
    uint32_t type;
    uint32_t flags;
    uint32_t addr;
    uint32_t offset;
    uint32_t size;
    uint32_t link;
    uint32_t info;
    uint32_t align;
    uint32_t esize;
});

PACK(struct elf_sym32
{
    uint32_t name;
    uint32_t value;
    uint32_t size;
    uint8_t info;
    uint8_t other;
    uint16_t shndx;
});

PACK(struct elf_dynamic
{
    size_t unused0;
    size_t unused1;
    size_t plt_rel_sz;
    size_t plt_got;
    size_t hash;
    size_t str_tab;
    size_t sym_tab;
    size_t rela;
    size_t rela_sz;
    size_t rela_ent;
    size_t str_sz;
    size_t sym_ent;
    size_t init;
    size_t fini;
    size_t soname;
    size_t rpath;
    size_t symbolic;
    size_t rel;
    size_t rel_sz;
    size_t rel_ent;
    size_t plt_rel;
    size_t debug;
    size_t text_rel;
    size_t jmp_rel;
    size_t reserved0;
    size_t init_array;
    size_t fini_array;
    size_t init_array_sz;
    size_t fini_array_sz;
    size_t reserved1;
    size_t reserved2;
    size_t reserved3;
});

#define ELF_TYPE_EXEC  2
#define ELF_TYPE_SHARED  3

#define ELF_MACH_I386  3

#define ELF_PH_LOAD  1
#define ELF_PH_DYNAMIC  2

#define ELF_DYN_NEEDED  1

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

uint8_t elf_sign[16] = { 0x7F, 'E', 'L', 'F', 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

int elf_check_header(elf_header_t *head, size_t page, int mach)
{
    if (head == NULL)
        return -1;
    if (memcmp(head->sign, elf_sign, 16) != 0)
        return -1;
    if (head->type != ELF_TYPE_EXEC && head->type != ELF_TYPE_SHARED)
        return -1;
    if (head->version == 0 || head->machine != mach)
        return -1;
    if (head->ph_off + head->ph_size > page)
        return -1;
    return 0;
}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

const char *rights[] = {
    "---", "--x", "-w-", "-wx",
    "r--", "r-x", "rw-", "rwx",
};

int elf_open(dlib_t *lib)
{
    int i;
    elf_header_t *head = lib->map(lib, 0);

    if (elf_check_header(head, lib->page, ELF_MACH_I386) != 0) {
        lib->umap(lib, head);
        return -1;
    }
    printf("ELF I386\n");
    printf("ELF ENTRY %p\n", (size_t)head->entry);
    lib->entry = head->entry;

    printf("ELF PH TABLE\n");
    elf_phead_t *ph_tbl = &((char*)head)[head->ph_off];
    int sections = 0;
    for (i = 0; i < head->ph_count; ++i) {
        if (ph_tbl[i].type == ELF_PH_LOAD) {
            printf("  LOAD %p %p %s\n", (size_t)ph_tbl[i].file_addr, (size_t)ph_tbl[i].virt_addr, rights[ph_tbl[i].flags & 7]);
            lib->sections[sections].offset = ph_tbl[i].file_addr;
            lib->sections[sections].physic = ph_tbl[i].virt_addr;
            lib->sections[sections].length = ph_tbl[i].virt_size;
            lib->sections[sections].flags = ph_tbl[i].flags;
            lib->sections[sections].flsize = ph_tbl[i].file_size;
            sections++;
        } else if (ph_tbl[i].type == ELF_PH_DYNAMIC) {
            printf("  DYNAMIC %p\n", (size_t)ph_tbl[i].file_addr);
        } else {
            printf("  OTHER (%d)\n", ph_tbl[i].type, (size_t)ph_tbl[i].file_addr);
        }
    }

    lib->umap(lib, head);
    return 0;
}

