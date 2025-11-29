// Forth16 main

#include <stdio.h>
#include <string.h>

#include "vm.h"

static u8 vmem[65536]; // 64K Forth memory

static const char blockfile[] = "blocks.fb"; // same as gforth

int main() {
    printf("Forth16 v1.0\n");

    if (disk_init(blockfile)) {
        fprintf(stderr, "Cannot open %s\n", blockfile);
        return 1;
    }

    // Load block 0 at address 0 and run the CPU
    if (disk_read(0, vmem)) {
        fprintf(stderr, "Cannot read boot block from %s\n", blockfile);
        return 2;
    }

    int rc = cpu_run(0, vmem);

    fprintf(stderr, "VM exit $%02X\n", rc);
    return rc;
}
