#pragma once

#include <stddef.h>

typedef struct desc dsec_t;
typedef struct dlib dlib_t;

struct desc
{
    size_t offset;
    size_t physic;
    size_t length;
    size_t flsize;
    int flags;
};

struct dlib
{
    void *inode;
    size_t page;
    size_t entry;
    dsec_t sections[4]; // Usually 2

    void *(*map)(dlib_t *, int);
    void (*umap)(dlib_t *, void *);
};


