// Disk I/O

#include <errno.h>
#include <fcntl.h>
#include <stdlib.h>
#include <unistd.h>

#include "vm.h"

static int block_fd = -1;

static int ioerr(ssize_t size) {
    if (size < 0) return errno;
    if (size < BLOCK_SIZE) return EIO;
    return 0;
}

int disk_init(const char* blockfile) {
    block_fd = open(blockfile, O_RDWR);
    return block_fd < 0 ? errno : 0;
}

int disk_read(int blk, void *buffer) {
    if (lseek(block_fd, blk * BLOCK_SIZE, SEEK_SET) < 0) return errno;
    return ioerr(read(block_fd, buffer, BLOCK_SIZE));
}

int disk_write(int blk, void *buffer) {
    if (lseek(block_fd, blk * BLOCK_SIZE, SEEK_SET) < 0) return errno;
    return ioerr(write(block_fd, buffer, BLOCK_SIZE));
}

int disk_flush(void) {
    return fdatasync(block_fd) ? errno : 0;
}
