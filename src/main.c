// Forth16 main

#include <stdio.h>
#include <string.h>

#include "vm.h"

static const char blockfile[] = "blocks.fb"; // same as gforth

int main() {
    printf("Forth16 v1.0\n");
    if (disk_init(blockfile, strlen(blockfile))) {
        fprintf(stderr, "Cannot open %s\n", blockfile);
        return 1;
    }
    if (disk_read(0, vmem)) {
        fprintf(stderr, "Cannot read boot block from %s\n", blockfile);
        return 2;
    }
//    cpu_reset();
    return 0;
}
