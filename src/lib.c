// Forth library functions

// necessary to boot, consider moving the Forth

#include <stdlib.h>
#include <string.h>
#include "vm.h"

char *malloc_cstr(const void *adr, int len) {
    char *cstr = malloc(len + 1);
    memcpy(cstr, adr, len);
    cstr[len] = 0;
    return cstr;
}
