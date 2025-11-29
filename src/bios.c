// BIOS calls

#include <stdlib.h>
#include <stdio.h>
#include "vm.h"

i16 *bios(i16 service, i16 *S, u8 *vmem) {
    switch (service) {
        case 0: // BYE
            exit(0);
        case 1: // TYPE ( a n -- )
            type(vmem + S[1], S[0]);
            return S+2;
        case 2: // ACCEPT ( a n1 -- n2 )
            S[1] = accept(vmem + S[1], S[0]);
            return S+1;
        case 3: // print top in hex for debugging
            printf("$%04X ", *S);
            return S+1;
        default:
            fprintf(stderr, "Invalid BIOS call %d\n", service);
            exit(1);
    }
}
