// Forth BIOS

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include <unistd.h>
// ssize_t write(int fildes, const void *buf, size_t nbyte);

int accept(void *buf, int size);
FILE *open_file(const char *str, int len, const char *mode);

int bios(int svc, int16_t *argv, uint8_t *vmem) {
    switch (svc) {
        case 0: exit(argv[0]);
        case 1: // TYPE
            fwrite(vmem + argv[1], argv[0], 1, stdout); 
            break;
        case 2: // ACCEPT
            return accept(vmem + argv[1], argv[0]); break;
        case 3: // OPEN-FILE
            return 0; // ???
            // have to manage a set of "file descriptors", or use unbuffered?
            break;
    }
    return 0;
}
