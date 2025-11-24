// Console

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <editline/readline.h>

#include "vm.h"

int accept(void *buf, int size) {
    const char* prompt = 0;
    char *line = readline(prompt);
    if (!line) return -1;

    int len = strlen(line);
    if (len && line[len-1] == '\n') len--;
    if (len > size) len = size;
    memcpy(buf, line, len);

    add_history(line);

    free(line);
    return len;
}

int type(void *buf, int len) {
    int written = fwrite(buf, len, 1, stdout);
    return written == len ? 0 : 1;
}
