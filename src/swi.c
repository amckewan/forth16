// System calls via SWI instruction

#include <stdlib.h>
#include "vm.h"

i16 swi(i16 service, i16 arg[]) {
    switch (service) {
        case 0: exit(arg[0]); // BYE
        case 1: return type(vmem + arg[1], arg[0]);
        case 2: return accept(vmem + arg[1], arg[0]);
    }
    return -1;
}
